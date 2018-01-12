using Com.Gmlan.Core.Model.System;
using System.Linq;

namespace Com.Gmlan.Core.System
{
    public interface ISettingService
    {

        string GetSettingValueById(int id);
        string GetSettingValueByKey(string key);
        T GetSettingByKey<T>(string key);
        void SetSetting<T>(string key, T value);
        bool GetSettingBooleanValueByKey(string key);

        IQueryable<Setting> QueryAllSettings();

        void InsertOrUpdateSetting(string key, string value);

        void DeleteSetting(int id);

        void DeleteSetting(string key);
    }
}
