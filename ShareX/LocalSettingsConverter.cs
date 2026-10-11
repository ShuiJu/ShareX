#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace ShareX;

// Read old local preferences; normal serialization writes only the current model.
public sealed class LocalSettingsConverter : CustomCreationConverter<object>
{
    public override bool CanConvert(Type objectType) => objectType == typeof(ApplicationConfig) ||
        objectType == typeof(TaskSettings) || objectType == typeof(TaskSettingsGeneral);

    public override object Create(Type objectType) => objectType == typeof(ApplicationConfig) ? new ApplicationConfig() :
        objectType == typeof(TaskSettings) ? new TaskSettings() :
        objectType == typeof(TaskSettingsGeneral) ? new TaskSettingsGeneral() :
        throw new JsonSerializationException($"Unsupported settings type: {objectType}.");

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject data = JObject.Load(reader);
        if (objectType == typeof(ApplicationConfig))
        {
            Move(data, "UploadLimit", data, nameof(ApplicationConfig.TaskLimit));
            Move(data, "AutoCaptureWaitUpload", data, nameof(ApplicationConfig.AutoCaptureWaitTasks));
        }
        else if (objectType == typeof(TaskSettingsGeneral))
        {
            Move(data, "PlaySoundAfterUpload", data, nameof(TaskSettingsGeneral.PlaySoundAfterTaskCompleted));
        }
        else if (data["UploadSettings"] is JObject naming)
        {
            JObject image = data[nameof(TaskSettings.ImageSettings)] as JObject;
            if (image == null)
            {
                image = new JObject();
                data[nameof(TaskSettings.ImageSettings)] = image;
            }
            Move(naming, nameof(TaskSettingsImage.NameFormatPattern), image, nameof(TaskSettingsImage.NameFormatPattern));
            Move(naming, nameof(TaskSettingsImage.NameFormatPatternActiveWindow), image, nameof(TaskSettingsImage.NameFormatPatternActiveWindow));
            Move(naming, nameof(TaskSettingsImage.UseCustomTimeZone), image, nameof(TaskSettingsImage.UseCustomTimeZone));
            Move(naming, nameof(TaskSettingsImage.CustomTimeZone), image, nameof(TaskSettingsImage.CustomTimeZone));
        }

        object settings = Create(objectType);
        using JsonReader migrated = data.CreateReader();
        serializer.Populate(migrated, settings);
        return settings;
    }

    private static void Move(JObject source, string oldName, JObject target, string newName)
    {
        JProperty property = source.Property(oldName);
        if (property == null || target.Property(newName) != null) return;
        JToken value = property.Value;
        property.Remove();
        property.Value = JValue.CreateNull();
        target[newName] = value;
    }
}
