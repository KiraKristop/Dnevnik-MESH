# DnevnikMESH By KiraKris — Unity

Готовые скрипты — аналог `DnevnikMESH_By_KiraKris` (WinForms) и `dnevniklib` (Python) для Unity. Бренд **DnevnikMESH By KiraKris**.

## Что внутри
- `Scripts/MeshUnityClient.cs` — клиент на `UnityWebRequest` (как `MeshClient.cs` / `student.py` + `marks.py` + `schedule.py`). Сам вытаскивает `aupd_token` даже если вставишь всю строку куков.
- `Scripts/MeshApi.cs` — модели
- `Scripts/MeshDiaryManager.cs` — MonoBehaviour для UI (кнопки Войти/Расписание/Оценки/ДЗ/Уведомления)

API уже пофикшен: старый `dnevnik.mos.ru/core/api/student_profiles/` дает 403, используется `school.mos.ru/api/family/web/v1/profile`.

## Как подключить в Unity (2021.3 LTS / 2022.3 LTS)

1. Создай новый проект Unity (2D, URP или 3D — любой).
2. Скопируй папку `Scripts` в `Assets/Scripts/MeshDiary/`.
3. Если нет TextMeshPro: `Window → TextMeshPro → Import TMP Essentials`.
4. Создай Canvas:
   - `GameObject → UI → Canvas`
   - Добавь: `InputField (TMP)` для токена, `Button` Войти, `Text (TMP)` для userInfo/status, 4 кнопки (Расписание/Оценки/ДЗ/Уведомления), `InputField` для дат (YYYY-MM-DD), `ScrollView` с `Text (TMP)` для лога.
5. Создай пустой `GameObject` "MeshManager", повесь на него `MeshDiaryManager.cs`, перетащи все поля в инспекторе.
6. Привяжи кнопки: `OnClick → MeshDiaryManager → OnClickLogin / OnClickSchedule` и т.д. (или оставь как в скрипте — он сам подписывает loginButton).

## Где взять токен
`school.mos.ru` → F12 → Network → Fetch/XHR → фильтр `userinfo` → Headers → Request Headers → `Authorization: Bearer eyJ...` или из Application → Cookies → `aupd_token`.

## Сборка
- Для Android: `File → Build Settings → Android → Switch Platform`. Добавь `INTERNET` permission (вкл по умолчанию).
- Для WebGL: работает, но `school.mos.ru` может требовать CORS — для WebGL лучше сделать прокси через свой Flask (`app.py` из этого репо).
- Токен сохраняется в `PlayerPrefs` (ключ `mesh_token`).

## Пример кода без UI
```csharp
var client = new MeshUnityClient("eyJhbGc...");
string user = await client.GetUserInfoRaw();
var (prof, sid) = await client.GetProfileRaw();
string marks = await client.GetMarksRaw("2026-09-01", "2026-09-06");
Debug.Log(marks);
```

Это 1-в-1 порт Python `Student(token)`, `Marks(student).get_marks_by_date()` и т.д.
