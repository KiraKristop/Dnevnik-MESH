// Модели для МЭШ — порт dnevniklib/types/* на C# для Unity
// Работает с JsonUtility / System.Text.Json в зависимости от версии Unity
using System;

[Serializable]
public class UserInfoResponse
{
    public UserInfoData info;
    public string login;
    public long userId;
}
[Serializable]
public class UserInfoData
{
    public string FirstName;
    public string LastName;
    public string MiddleName;
    public string mail;
    public string birthdate;
    public string mobile;
}
[Serializable]
public class ProfileResponse
{
    public ProfileData profile;
    public ChildData[] children;
}
[Serializable]
public class ChildData
{
    public long id;
    public string class_name;
    public string contingent_guid;
    public int age;
    public string sex;
    public SchoolData school;
}
[Serializable]
public class SchoolData { public int id; public string short_name; public string name; }

[Serializable]
public class MarkData
{
    public long id;
    public string value;
    public string comment;
    public string subject_name;
    public int subject_id;
    public string control_form_name;
    public int weight;
    public string created_at;
    public bool is_exam;
}
[Serializable]
public class HomeworkData
{
    public long id;
    public string description;
    public int subject_id;
    public string subject_name;
    public string created_at;
    public bool is_done;
}
