using System.Windows;
using System.Windows.Markup;
using Sundew.Xaml.Controls;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsDefinition(Constants.SundewXamlXmlNamespace, "Sundew.Xaml.Controls")]
[assembly: XmlnsPrefix(Constants.SundewXamlXmlNamespace, "sx")]