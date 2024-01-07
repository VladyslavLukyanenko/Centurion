// using Centurion.Accounts.Core.Audit.Mappings;
// using Centurion.Accounts.Core.Products;
// using Centurion.Accounts.Infra.Audit.EntryValueConverters;
//
// namespace Centurion.Accounts.Infra.Products.AuditMapping
// {
//   public class ProductAuditMapping : AuditMappingBase
//   {
//     public ProductAuditMapping()
//     {
//       Map<Product, long>()
//         .Property(_ => _.Name)
//         .Property(_ => _.Description)
//         .Property(_ => _.DiscordGuildId)
//         .Property(_ => _.DiscordRoleId)
//         .Property(_ => _.Id, typeof(UserIdToFullNameEntryValueConverter));
//     }
//   }
// }