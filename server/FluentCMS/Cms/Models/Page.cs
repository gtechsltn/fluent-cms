namespace FluentCMS.Cms.Models;

public sealed class Page
{
    
    public string Name { get; set; } = "";
    public string Query { get; set; } = "";
    public string QueryString { get; set; } = "";
    
    public string Html { get; set; } = "";
    public string Css { get; set; } = "";

    public const string HomePage = "home";
    public static string SinglePageName(string pageName)
    {
        return pageName + "/single";
    } 
    
    /*for grapes.js restore last configure */
    public string Components { get; set; } = "";
    public string Styles { get; set; } = "";
}