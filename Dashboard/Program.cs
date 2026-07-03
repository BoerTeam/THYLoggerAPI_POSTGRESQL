using System.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
public partial class Program
{


    public static string Service_Link = "";
    private static void readConfig()
    {
        string current = "";
        XmlTextReader xmlTextReader = new XmlTextReader("Webconfig.xml");

        while (xmlTextReader.Read())
        {
            if (xmlTextReader.NodeType == XmlNodeType.Element)
            {
                current = xmlTextReader.LocalName;
            }
            if (xmlTextReader.NodeType == XmlNodeType.Text)
            {
                if (current == "ServiceLink")
                {
                    Program.Service_Link = xmlTextReader.Value.Trim();
                }
            }
        }
        xmlTextReader.Close();
    }
}