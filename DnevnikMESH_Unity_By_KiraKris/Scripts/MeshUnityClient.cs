using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

// Клиент МЭШ для Unity — аналог MeshDiary/MeshClient.cs и dnevniklib/student/student.py
// Использует UnityWebRequest (работает и в WebGL, и на Android/iOS)
// Токен = aupd_token (eyJ...), можно вставлять даже всю строку куков — сам вытащит
public class MeshUnityClient
{
    private string token;
    public long StudentId { get; private set; }
    public string ClassName { get; private set; } = "";
    public string SchoolName { get; private set; } = "";

    public MeshUnityClient(string rawToken)
    {
        token = ExtractToken(rawToken);
    }

    public static string ExtractToken(string raw)
    {
        raw = raw.Trim();
        if (raw.StartsWith("Bearer ")) raw = raw.Substring(7);
        if (raw.Contains("aupd_token="))
        {
            int s = raw.IndexOf("aupd_token=") + 11;
            int e = raw.IndexOf(";", s);
            if (e == -1) e = raw.Length;
            raw = raw.Substring(s, e - s).Trim();
        }
        return raw;
    }

    // Универсальный GET
    private async Task<string> GetAsync(string url, bool isMobile = false)
    {
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Auth-Token", token);
        req.SetRequestHeader("X-Mes-Subsystem", isMobile ? "familymp" : "familyweb");
        if (isMobile && StudentId != 0) req.SetRequestHeader("Profile-Id", StudentId.ToString());
        // для userinfo нужен Bearer
        if (url.Contains("v3/userinfo"))
        {
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }
        req.downloadHandler = new DownloadHandlerBuffer();
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();
#if UNITY_2020_1_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
            throw new System.Exception($"{url} {req.responseCode}: {req.error} {req.downloadHandler.text}");
        return req.downloadHandler.text;
    }

    public Task<string> GetUserInfoRaw() => GetAsync("https://school.mos.ru/v3/userinfo");
    public async Task<(string json, long studentId)> GetProfileRaw()
    {
        string json = await GetAsync("https://school.mos.ru/api/family/web/v1/profile");
        // парсим без зависимостей — ищем "id":13435541
        // для надежности используем JsonUtility через мини-парсинг SimpleJSON, но тут делаем через поиск
        // лучше подключить Newtonsoft.Json если есть, иначе так:
        long sid = 0;
        try
        {
            // пробуем через System.Text.Json если доступно (Unity 2021+)
#if UNITY_2021_2_OR_NEWER
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            System.Text.Json.JsonElement child;
            if (root.TryGetProperty("children", out var children) && children.GetArrayLength() > 0)
                child = children[0];
            else
                child = root.GetProperty("profile");
            sid = child.GetProperty("id").GetInt64();
            ClassName = child.TryGetProperty("class_name", out var cn) ? cn.GetString() ?? "" : "";
            if (child.TryGetProperty("school", out var school) && school.ValueKind != System.Text.Json.JsonValueKind.Null)
                SchoolName = school.TryGetProperty("short_name", out var sn) ? sn.GetString() ?? "" : "";
#else
            // fallback — простой поиск строки
            int idx = json.IndexOf("\"id\":");
            if (idx != -1) { int end = json.IndexOf(",", idx); sid = long.Parse(json.Substring(idx+5, end-idx-5)); }
#endif
        }
        catch { }
        StudentId = sid;
        return (json, sid);
    }

    public Task<string> GetScheduleRaw(string date)
    {
        if (StudentId==0) throw new System.Exception("Сначала GetProfileRaw()");
        return GetAsync($"https://school.mos.ru/api/family/web/v1/schedule?student_id={StudentId}&date={date}");
    }
    public Task<string> GetMarksRaw(string from, string to)
    {
        if (StudentId==0) throw new System.Exception("Сначала GetProfileRaw()");
        return GetAsync($"https://school.mos.ru/api/family/web/v1/marks?student_id={StudentId}&from={from}&to={to}");
    }
    public Task<string> GetHomeworksRaw(string from, string to)
    {
        if (StudentId==0) throw new System.Exception("Сначала GetProfileRaw()");
        return GetAsync($"https://school.mos.ru/api/family/web/v1/homeworks?from={from}&to={to}&student_id={StudentId}");
    }
    public Task<string> GetNotificationsRaw()
    {
        if (StudentId==0) throw new System.Exception("Сначала GetProfileRaw()");
        return GetAsync($"https://school.mos.ru/api/family/mobile/v1/notifications/search?student_id={StudentId}", isMobile:true);
    }
}
