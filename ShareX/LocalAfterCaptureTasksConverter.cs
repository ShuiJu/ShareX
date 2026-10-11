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
using System;
using System.Globalization;

namespace ShareX;

public sealed class LocalAfterCaptureTasksConverter : StringEnumConverter
{
    private static readonly AfterCaptureTasks SupportedTasks = GetSupportedTasks();

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return (AfterCaptureTasks)Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture) & SupportedTasks;
        }

        if (reader.TokenType == JsonToken.String && reader.Value is string text)
        {
            AfterCaptureTasks tasks = AfterCaptureTasks.None;
            ReadOnlySpan<char> remaining = text.AsSpan();
            while (!remaining.IsEmpty)
            {
                int separator = remaining.IndexOf(',');
                ReadOnlySpan<char> part = separator < 0 ? remaining : remaining[..separator];
                if (Enum.TryParse(part.Trim(), true, out AfterCaptureTasks task)) tasks |= task & SupportedTasks;
                if (separator < 0) break;
                remaining = remaining[(separator + 1)..];
            }
            return tasks;
        }

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    private static AfterCaptureTasks GetSupportedTasks()
    {
        AfterCaptureTasks tasks = AfterCaptureTasks.None;
        foreach (AfterCaptureTasks task in Enum.GetValues<AfterCaptureTasks>()) tasks |= task;
        return tasks;
    }
}
