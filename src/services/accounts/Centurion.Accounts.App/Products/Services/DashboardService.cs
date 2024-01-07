using AutoMapper;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.FileStorage.FileSystem;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Config;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.App.Products.Services;

public class DashboardService : IDashboardService
{
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IMapper _mapper;
  private readonly DashboardsConfig _config;
  private readonly IFileUploadService _fileUploadService;

  public DashboardService(IDashboardRepository dashboardRepository, IMapper mapper, DashboardsConfig config,
    IFileUploadService fileUploadService)
  {
    _dashboardRepository = dashboardRepository;
    _mapper = mapper;
    _config = config;
    _fileUploadService = fileUploadService;
  }

  public async ValueTask UpdateAsync(Dashboard dashboard, DashboardData cmd, CancellationToken ct = default)
  {
    _mapper.Map(cmd, dashboard);
    _mapper.Map(cmd.DiscordConfig, dashboard.DiscordConfig);
    _mapper.Map(cmd.StripeConfig, dashboard.StripeConfig);
    _mapper.Map(cmd.HostingConfig, dashboard.HostingConfig);

    await FillProductInfoAsync(dashboard.ProductInfo, cmd.ProductInfo, ct);

    _dashboardRepository.Update(dashboard);
  }

  private async ValueTask<ProductFeature[]> CreateProductFeaturesAsync(IList<ProductFeatureData> featuresData,
    CancellationToken ct = default)
  {
    var featureTasks = featuresData.Select(async f =>
    {
      f.Icon = await _fileUploadService.UploadFileOrDefaultAsync(f.UploadedIcon, _config.FeatureIconUploadConfig,
        f.Icon, ct);

      return new ProductFeature(f.Icon, f.Title, f.Desc);
    });

    var features = await Task.WhenAll(featureTasks);
    return features;
  }

  private async ValueTask FillProductInfoAsync(ProductInfo product, ProductInfoData data,
    CancellationToken ct = default)
  {
    _mapper.Map(data, product);
    product.LogoSrc = await _fileUploadService
      .UploadFileOrDefaultAsync(data.UploadedLogo, _config.LogoUploadConfig, product.LogoSrc, ct);

    product.ImageSrc = await _fileUploadService
      .UploadFileOrDefaultAsync(data.UploadedImage, _config.ImageUploadConfig, product.ImageSrc, ct);
    product.Features = await CreateProductFeaturesAsync(data.Features, ct);
  }
}