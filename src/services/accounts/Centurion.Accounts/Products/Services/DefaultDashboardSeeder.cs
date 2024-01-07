using System.Text;
using Centurion.Accounts.App;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra;
using Microsoft.EntityFrameworkCore;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace Centurion.Accounts.Products.Services;

public class DefaultDashboardSeeder : IDataSeeder
{
  private const string DebugDashboard = "centurion";
  private readonly IDashboardRepository _dashboardRepository;
  private readonly DbContext _context;
  private readonly IWebHostEnvironment _hostingEnvironment;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserManager _userManager;


  public DefaultDashboardSeeder(IDashboardRepository dashboardRepository, DbContext context,
    IWebHostEnvironment hostingEnvironment, IUnitOfWork unitOfWork, IUserManager userManager)
  {
    _dashboardRepository = dashboardRepository;
    _context = context;
    _hostingEnvironment = hostingEnvironment;
    _unitOfWork = unitOfWork;
    _userManager = userManager;
  }

  public int Order => int.MaxValue;

  public async Task SeedAsync()
  {
    if (await _context.Set<Dashboard>()
          .AsQueryable()
          .AnyAsync(_ => _.HostingConfig.DomainName == DebugDashboard))
    {
      return;
    }


    var options = new CsvParserOptions(true, ',');
    var mapping = new SetupUserMapping();
    var parser = new CsvParser<SetupDashboard>(options, mapping);

    var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Setup", "Dashboards.csv");
    var results = parser.ReadFromFile(filePath, Encoding.UTF8)
      .ToList();

    if (!results.All(_ => _.IsValid))
    {
      throw new AppException("Can't read setup dashboards data");
    }

    var dashboards = results
      .Select(_ => _.Result)
      .ToLookup(_ => _.Email);

    foreach (var group in dashboards)
    {
      var setupUser = group.First();
      User user = User.CreateWithDiscordId(setupUser.Email, setupUser.Username, setupUser.DiscordUserId,
        setupUser.Avatar,
        setupUser.Discriminator);
      await _userManager.CreateAsync(user);
      var userDashboards = dashboards[setupUser.Email];
      foreach (var setupDashboard in userDashboards)
      {
        var hostingConfig = new HostingConfig
        {
          Mode = setupDashboard.DomainMode,
          DomainName = setupDashboard.Domain
        };

        var dashboard = new Dashboard(user.Id, setupDashboard.DiscordGuildId,
          setupDashboard.DiscordBotAccessToken, hostingConfig)
        {
          DiscordConfig =
          {
            OAuthConfig =
            {
              Scope = setupDashboard.DiscordScopes,
              ClientId = setupDashboard.DiscordClientId,
              ClientSecret = setupDashboard.DiscordClientSecret,
              RedirectUrl = setupDashboard.DiscordRedirectUrl
            }
          }
        };

        await _dashboardRepository.CreateAsync(dashboard);
      }
    }

    await _unitOfWork.SaveEntitiesAsync();
  }

  private class SetupDashboard
  {
    public string Username { get; set; } = null!;
    public string Discriminator { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ulong DiscordUserId { get; set; }
    public string Avatar { get; set; } = null!;
    public string DiscordBotAccessToken { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public DashboardHostingMode DomainMode { get; set; }
    public ulong DiscordGuildId { get; set; }
    public string DiscordClientId { get; set; } = null!;
    public string DiscordClientSecret { get; set; } = null!;
    public string DiscordRedirectUrl { get; set; } = null!;
    public string DiscordScopes { get; set; } = null!;
  }

  private class SetupUserMapping : CsvMapping<SetupDashboard>
  {
    public SetupUserMapping()
    {
      MapProperty(0, _ => _.Username);
      MapProperty(1, _ => _.Discriminator);
      MapProperty(2, _ => _.Email);
      MapProperty(3, _ => _.DiscordUserId);
      MapProperty(4, _ => _.Avatar);
      MapProperty(5, _ => _.DiscordBotAccessToken);
      MapProperty(6, _ => _.Domain);
      MapProperty(7, _ => _.DomainMode, new EnumConverter<DashboardHostingMode>(true));
      MapProperty(8, _ => _.DiscordGuildId);
      MapProperty(9, _ => _.DiscordClientId);
      MapProperty(10, _ => _.DiscordClientSecret);
      MapProperty(11, _ => _.DiscordRedirectUrl);
      MapProperty(12, _ => _.DiscordScopes);
    }
  }
}