﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HedgeModManager.Controls;
using Newtonsoft.Json;

namespace HedgeModManager.UI
{
    public class FormBuilder
    {
        public static Dictionary<string, Type> TypeDatabase = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "string", typeof(string) },
            { "int", typeof(long) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "bool", typeof(bool) }
        };

        public static Thickness GroupMargin = new Thickness(0, 0, 0, 15);

        public static Panel Build(FormSchema schema)
        {
            var stack = new StackPanel();
            foreach (var group in schema.Groups)
            {
                var box = new GroupBox(){ Name = group.Name, Header = group.DisplayName, Margin = GroupMargin};
                var panel = new StackPanel();
                box.Content = panel;
                foreach (var element in group.Elements)
                {
                    panel.Children.Add(new FormItem(element, schema));
                }

                stack.Children.Add(box);
            }

            return stack;
        }
    }

    public class FormElement
    {
        public string Name;
        public List<string> Description = new List<string>();
        public string DisplayName;
        public string Type;
        public double? MinValue;
        public double? MaxValue;
        public dynamic DefaultValue;

        [JsonIgnore]
        public dynamic Value { get; set; }

        [JsonIgnore]
        public long ValueLong
        {
            get => Value;
            set => Value = value;
        }

        [JsonIgnore]
        public double ValueDouble
        {
            get => Value;
            set => Value = value;
        }
    }

    public class FormGroup
    {
        public string Name;
        public string DisplayName;
        public List<FormElement> Elements = new List<FormElement>();
    }

    public class FormEnum
    {
        public string DisplayName;
        public string Value;
        public List<string> Description = new List<string>();

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class FormSchema
    {
        public string IniFile { get; set; } = "Config.ini";
        public List<FormGroup> Groups = new List<FormGroup>();
        public Dictionary<string, List<FormEnum>> Enums = new Dictionary<string, List<FormEnum>>();

        public void LoadValuesFromIni(string path)
        {
            var file = new IniFile(path);
            foreach (var group in Groups)
            {
                foreach (var element in group.Elements)
                {
                    if (FormBuilder.TypeDatabase.TryGetValue(element.Type, out var t))
                    {
                        if (file.Groups.ContainsKey(group.Name) && file[group.Name].Params.ContainsKey(element.Name))
                        {
                            element.Value = Convert.ChangeType(file[group.Name][element.Name], t);
                        }
                    }
                    else if (file.Groups.ContainsKey(group.Name) && file[group.Name].Params.ContainsKey(element.Name))
                    {
                        element.Value = file[group.Name][element.Name];
                    }
                }
            }
        }

        public void SaveIni(string path)
        {
            var file = new IniFile();
            foreach (var group in Groups)
            {
                foreach (var element in group.Elements)
                {
                    if (element.Value is FrameworkElement && ((FrameworkElement)element.Value).DataContext is FormEnum)
                        file[group.Name][element.Name] = ((FormEnum)((FrameworkElement)element.Value).DataContext)?.Value;
                    else
                        file[group.Name][element.Name] = element.Value?.ToString() ?? element.DefaultValue.ToString();
                }
            }
            using (var stream = File.Create(Path.Combine(path)))
            {
                file.Write(stream);
            }
        }
    }
}
