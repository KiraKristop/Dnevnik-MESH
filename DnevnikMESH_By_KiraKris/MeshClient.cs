using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeshDiary;

public class MeshClient
{
    private readonly HttpClient _http = new();
    private readonly string _token;
    public long StudentId { get; private set; }
    public string ClassName { get; private set; } = "";
    public string SchoolName { get; private set; } = "";
    public string PersonGuid { get; private set; } = "";

    public MeshClient(string token)
    {
        if (token.StartsWith("Bearer ")) token = token[7..];
        _token = token.Trim();
        _http.DefaultRequestHeaders.Add("Auth-Token", _token);
        _http.DefaultRequestHeaders.Add("X-Mes-Subsystem", "familyweb");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<UserInfo> GetUserInfoAsync()
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://school.mos.ru/v3/userinfo");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        req.Headers.Remove("Auth-Token");
        req.Headers.Remove("X-Mes-Subsystem");
        var res = await _http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"userinfo {res.StatusCode}: {body}");
        return JsonSerializer.Deserialize<UserInfo>(body)!;
    }

    public async Task<ProfileResult> GetProfileAsync()
    {
        var res = await _http.GetAsync("https://school.mos.ru/api/family/web/v1/profile");
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"profile {res.StatusCode}: {body}");
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        // у ученика - profile, у родителя - children[0]
        JsonElement child;
        if (root.TryGetProperty("children", out var children) && children.GetArrayLength() > 0)
            child = children[0];
        else if (root.TryGetProperty("profile", out var profile))
            child = profile;
        else
            throw new Exception("Не найден profile/children в ответе");

        StudentId = child.GetProperty("id").GetInt64();
        ClassName = child.TryGetProperty("class_name", out var cn) ? cn.GetString() ?? "" : "";
        PersonGuid = child.TryGetProperty("contingent_guid", out var cg) ? cg.GetString() ?? "" : "";
        if (child.TryGetProperty("school", out var school) && school.ValueKind == JsonValueKind.Object)
            SchoolName = school.TryGetProperty("short_name", out var sn) ? sn.GetString() ?? "" : "";

        return new ProfileResult
        {
            StudentId = StudentId,
            ClassName = ClassName,
            SchoolName = SchoolName,
            ChildJson = child.GetRawText(),
            RawJson = body
        };
    }

    public async Task<JsonDocument> GetScheduleAsync(string date)
    {
        var url = $"https://school.mos.ru/api/family/web/v1/schedule?student_id={StudentId}&date={date}";
        var res = await _http.GetAsync(url);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"{res.StatusCode}: {body}");
        return JsonDocument.Parse(body);
    }

    public async Task<JsonDocument> GetMarksAsync(string from, string to)
    {
        var url = $"https://school.mos.ru/api/family/web/v1/marks?student_id={StudentId}&from={from}&to={to}";
        var res = await _http.GetAsync(url);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"{res.StatusCode}: {body}");
        return JsonDocument.Parse(body);
    }

    public async Task<JsonDocument> GetHomeworksAsync(string from, string to)
    {
        var url = $"https://school.mos.ru/api/family/web/v1/homeworks?from={from}&to={to}&student_id={StudentId}";
        var res = await _http.GetAsync(url);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"{res.StatusCode}: {body}");
        return JsonDocument.Parse(body);
    }

    public async Task<JsonDocument> GetNotificationsAsync()
    {
        using var req = new HttpRequestMessage(HttpMethod.Get,
            $"https://school.mos.ru/api/family/mobile/v1/notifications/search?student_id={StudentId}");
        req.Headers.Remove("X-Mes-Subsystem");
        req.Headers.Add("X-Mes-Subsystem", "familymp");
        req.Headers.Add("Profile-Id", StudentId.ToString());
        var res = await _http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"{res.StatusCode}: {body}");
        return JsonDocument.Parse(body);
    }
}

public class UserInfo
{
    [JsonPropertyName("info")]
    public Info Info { get; set; } = new();
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";
    [JsonPropertyName("userId")]
    public long UserId { get; set; }
}

public class Info
{
    [JsonPropertyName("FirstName")] public string FirstName { get; set; } = "";
    [JsonPropertyName("LastName")] public string LastName { get; set; } = "";
    [JsonPropertyName("MiddleName")] public string MiddleName { get; set; } = "";
    [JsonPropertyName("mail")] public string Mail { get; set; } = "";
    [JsonPropertyName("birthdate")] public string Birthdate { get; set; } = "";
    [JsonPropertyName("mobile")] public string Mobile { get; set; } = "";
}

public class ProfileResult
{
    public long StudentId { get; set; }
    public string ClassName { get; set; } = "";
    public string SchoolName { get; set; } = "";
    public string ChildJson { get; set; } = "";
    public string RawJson { get; set; } = "";
}
