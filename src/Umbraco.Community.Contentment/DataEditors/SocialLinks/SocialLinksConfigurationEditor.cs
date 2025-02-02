﻿/* Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
#if NET472
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using UmbConstants = Umbraco.Core.Constants;
#else
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using UmbConstants = Umbraco.Cms.Core.Constants;
#endif

namespace Umbraco.Community.Contentment.DataEditors
{
    internal sealed class SocialLinksConfigurationEditor : ConfigurationEditor
    {
        internal const string OverlayView = "overlayView";

        private readonly IIOHelper _ioHelper;

        public SocialLinksConfigurationEditor(IIOHelper ioHelper)
            : base()
        {
            _ioHelper = ioHelper;

            DefaultConfiguration.Add("networks", new[]
            {
                new { key = "network", value = new { network = "facebook", name = "Facebook", url = "https://facebook.com/", icon = "icon-facebook", backgroundColor = "#3b5998", iconColor = "#fff" } },
                new { key = "network", value = new { network = "twitter", name = "Twitter", url = "https://twitter.com/", icon = "icon-twitter", backgroundColor = "#2795e9", iconColor = "#fff" } },
                new { key = "network", value = new { network = "instagram", name = "Instagram", url = "https://instagram.com/", icon = "icon-instagram", backgroundColor = "#305777", iconColor = "#fff" } },
                new { key = "network", value = new { network = "linkedin", name = "LinkedIn", url = "https://linkedin.com/in/", icon = "icon-linkedin", backgroundColor = "#007bb6", iconColor = "#fff" } },
                new { key = "network", value = new { network = "youtube", name = "YouTube", url = "https://www.youtube.com/channel/", icon = "icon-youtube", backgroundColor = "#f00", iconColor = "#fff" } },
                new { key = "network", value = new { network = "github", name = "GitHub", url = "https://github.com/", icon = "icon-github", backgroundColor = "#000", iconColor = "#fff" } },
            });

            var items = new[]
            {
                new ConfigurationEditorModel
                {
                    Key = "network",
                    Name = "Social network",
                    Description = string.Empty,
                    Icon = SocialLinksDataEditor.DataEditorIcon,
                    DefaultValues = new Dictionary<string, object>
                    {
                        { "icon", UmbConstants.Icons.DefaultIcon },
                    },
                    Expressions = new Dictionary<string, string>
                    {
                        { "name", "{{ name }}" },
                        { "description", "{{ url }}" },
                        { "icon", "{{ icon.split(\" \")[0] }}" },
                        { "cardStyle", "{ \"background-color\": \"{{ backgroundColor }}\" }" },
                        { "iconStyle", "{ \"color\": \"{{ iconColor }}\" }" },
                    },
                    Fields = new[]
                    {
                        new ConfigurationField
                        {
                            Key = "network",
                            Name = "Network",
                            Description = "An alias for the social network. This will be used as the value of the selection.",
                            View = "textstring",
                        },
                        new ConfigurationField
                        {
                            Key = "name",
                            Name = "Name",
                            Description = "This will be used as the label of the social network in selection modal.",
                            View = "textstring",
                        },
                        new ConfigurationField
                        {
                            Key = "url",
                            Name = "Base URL",
                            Description = "This will be the starting part of the social network's profile URL.",
                            View = "textstring",
                        },
                        new ConfigurationField
                        {
                            Key = "icon",
                            Name = "Icon",
                            Description = "Typically select the logo for the social network.",
                            View = ioHelper.ResolveRelativeOrVirtualUrl("~/umbraco/views/propertyeditors/listview/icon.prevalues.html"),
                        },
                        new NotesConfigurationField(ioHelper, $@"<details class=""alert alert-info"">
<summary>Would you like to use a <strong>custom icon</strong>?</summary>
<p>To add your own custom icons to the Umbraco backoffice, add any SVG icon files to a custom plugin folder, e.g. <code>~/App_Plugins/[YourPluginName]/backoffice/icons/</code>.</p>
<p>For a step-by-step guide, Warren Buckley has a video tutorial: <a href=""https://www.youtube.com/watch?v=m90uxZBVFOw"" target=""_blank""><strong>How to Add Custom SVG icons to Umbraco Icon Picker</strong></a>.</p>
</details>", true),
                        new ConfigurationField
                        {
                            Key = "backgroundColor",
                            Name = "Background color",
                            Description = "The background color for the icon.",
                            View = ioHelper.ResolveRelativeOrVirtualUrl("~/umbraco/views/propertyeditors/eyedropper/eyedropper.html"),
                        },
                        new ConfigurationField
                        {
                            Key = "iconColor",
                            Name = "Icon color",
                            Description = "The foreground color of the icon.",
                            View = ioHelper.ResolveRelativeOrVirtualUrl("~/umbraco/views/propertyeditors/eyedropper/eyedropper.html"),
                        },
                    },
                    OverlaySize = OverlaySize.Medium,
                }
            };

            Fields.Add(new ConfigurationField
            {
                Key = "networks",
                Name = "Social networks",
                Description = "Define the icon set for the available social networks.",
                View = ioHelper.ResolveRelativeOrVirtualUrl(ConfigurationEditorDataEditor.DataEditorViewPath),
                Config = new Dictionary<string, object>
                {
                    { "allowDuplicates", Constants.Values.True },
                    { Constants.Conventions.ConfigurationFieldAliases.OverlayView, ioHelper.ResolveRelativeOrVirtualUrl(ConfigurationEditorDataEditor.DataEditorOverlayViewPath) },
                    { "displayMode", "cards" },
                    { Constants.Conventions.ConfigurationFieldAliases.Items, items },
                    { EnableDevModeConfigurationField.EnableDevMode, Constants.Values.True },
                }
            });

            Fields.Add(new ConfigurationField
            {
                Key = "confirmRemoval",
                Name = "Confirm removals?",
                Description = "Select to enable a confirmation prompt when removing an item.",
                View = "boolean",
            });

            DefaultConfiguration.Add(MaxItemsConfigurationField.MaxItems, 0);
            Fields.Add(new MaxItemsConfigurationField(ioHelper));
            Fields.Add(new EnableDevModeConfigurationField());
        }

        public override object FromConfigurationEditor(IDictionary<string, object> editorValues, object configuration)
        {
            if (editorValues.TryGetValueAs("networks", out JArray networks) == true)
            {
                foreach (JObject network in networks)
                {
                    var networkValue = network.GetValueAs("value", default(JObject));
                    if (networkValue?.ContainsKey("icon") == true)
                    {
                        var icon = networkValue.GetValueAs("icon", string.Empty);
                        if (string.IsNullOrWhiteSpace(icon) == false && icon.InvariantContains(" color-") == true)
                        {
                            networkValue["icon"] = icon.Split(new[] { ' ' })[0];
                        }
                    }
                }
            }

            return base.FromConfigurationEditor(editorValues, configuration);
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var config = base.ToValueEditor(configuration);

            if (config.TryGetValueAs("networks", out JArray array1) && array1.Count > 0)
            {
                var networks = new List<JObject>();

                for (var i = 0; i < array1.Count; i++)
                {
                    networks.Add((JObject)array1[i]["value"]);
                }

                config["networks"] = networks;
            }

            if (config.ContainsKey(OverlayView) == false)
            {
                config.Add(OverlayView, _ioHelper.ResolveRelativeOrVirtualUrl(SocialLinksDataEditor.DataEditorOverlayViewPath));
            }

            return config;
        }
    }
}
