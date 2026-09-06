using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

// Вешается на GameObject в сцене. Привяжи поля в инспекторе.
// UI: InputField для токена, Text для инфо, Buttons + ScrollView для вывода
// Аналог Form1.cs из MeshDiary (C# WinForms) но на Unity UGUI

public class MeshDiaryManager : MonoBehaviour
{
    [Header("DnevnikMESH By KiraKris — вставь aupd_token (eyJ...)")]
    public TMP_InputField tokenInput;
    public Button loginButton;
    public TMP_Text userInfoText;
    public TMP_Text statusText;

    [Header("Даты")]
    public TMP_InputField scheduleDate; // YYYY-MM-DD или оставь пустым = сегодня
    public TMP_InputField marksFrom;
    public TMP_InputField marksTo;
    public TMP_InputField hwFrom;
    public TMP_InputField hwTo;

    [Header("Вывод — можно один общий лог")]
    public TMP_Text logText;
    public ScrollRect logScroll;

    private MeshUnityClient client;

    void Start()
    {
        // подгружаем сохраненный токен
        if (PlayerPrefs.HasKey("mesh_token")) tokenInput.text = PlayerPrefs.GetString("mesh_token");
        if (scheduleDate) scheduleDate.text = System.DateTime.Today.ToString("yyyy-MM-dd");
        if (marksFrom) marksFrom.text = System.DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
        if (marksTo) marksTo.text = System.DateTime.Today.ToString("yyyy-MM-dd");
        if (hwFrom) hwFrom.text = System.DateTime.Today.ToString("yyyy-MM-dd");
        if (hwTo) hwTo.text = System.DateTime.Today.ToString("yyyy-MM-dd");

        if (loginButton) loginButton.onClick.AddListener(async () => await DoLogin());
    }

    public async void OnClickLogin() => await DoLogin();
    public async void OnClickSchedule() => await LoadSchedule();
    public async void OnClickMarks() => await LoadMarks();
    public async void OnClickHomeworks() => await LoadHomeworks();
    public async void OnClickNotifications() => await LoadNotifications();

    async Task DoLogin()
    {
        string raw = tokenInput.text.Trim();
        if (string.IsNullOrEmpty(raw)) { SetStatus("Вставь токен", true); return; }
        SetStatus("Проверяю...", false);
        try
        {
            client = new MeshUnityClient(raw);
            string userJson = await client.GetUserInfoRaw();
            var (profJson, sid) = await client.GetProfileRaw();
            PlayerPrefs.SetString("mesh_token", MeshUnityClient.ExtractToken(raw));
            PlayerPrefs.Save();

            // покажем коротко — полный json в лог
            userInfoText.text = $"ID {sid} • {client.ClassName} {client.SchoolName}";
            Log("USERINFO:\n" + userJson);
            Log("PROFILE:\n" + profJson);
            SetStatus($"Успешно! StudentId={sid}", false);
        }
        catch (System.Exception ex)
        {
            SetStatus("Ошибка: " + ex.Message, true);
            Log("LOGIN ERROR: " + ex);
        }
    }

    async Task LoadSchedule()
    {
        if (!EnsureClient()) return;
        string date = string.IsNullOrEmpty(scheduleDate.text) ? System.DateTime.Today.ToString("yyyy-MM-dd") : scheduleDate.text;
        try
        {
            string json = await client.GetScheduleRaw(date);
            Log($"SCHEDULE {date}:\n{json}");
            SetStatus($"Расписание {date} загружено", false);
        }
        catch (System.Exception ex) { SetStatus(ex.Message, true); Log(ex.ToString()); }
    }

    async Task LoadMarks()
    {
        if (!EnsureClient()) return;
        string from = marksFrom.text, to = marksTo.text;
        try
        {
            string json = await client.GetMarksRaw(from, to);
            Log($"MARKS {from}→{to}:\n{json}");
            SetStatus("Оценки загружены", false);
        }
        catch (System.Exception ex) { SetStatus(ex.Message, true); Log(ex.ToString()); }
    }

    async Task LoadHomeworks()
    {
        if (!EnsureClient()) return;
        string from = hwFrom.text, to = hwTo.text;
        try
        {
            string json = await client.GetHomeworksRaw(from, to);
            Log($"HOMEWORKS {from}→{to}:\n{json}");
            SetStatus("Домашка загружена", false);
        }
        catch (System.Exception ex) { SetStatus(ex.Message, true); Log(ex.ToString()); }
    }

    async Task LoadNotifications()
    {
        if (!EnsureClient()) return;
        try
        {
            string json = await client.GetNotificationsRaw();
            Log("NOTIFICATIONS:\n" + json);
            SetStatus("Уведомления загружены", false);
        }
        catch (System.Exception ex) { SetStatus(ex.Message, true); Log(ex.ToString()); }
    }

    bool EnsureClient()
    {
        if (client == null || client.StudentId == 0) { SetStatus("Сначала нажми Войти", true); return false; }
        return true;
    }
    void SetStatus(string msg, bool isError)
    {
        if (statusText) { statusText.text = msg; statusText.color = isError ? Color.red : Color.green; }
        Debug.Log(msg);
    }
    void Log(string msg)
    {
        if (logText) logText.text = msg + "\n\n" + logText.text;
        Debug.Log(msg);
        if (logScroll) Canvas.ForceUpdateCanvases();
    }
}
