// using System;
// using System.Collections.Generic;
// using DynamicData.Binding;
// using Newtonsoft.Json;
//
// namespace Centurion.Cli.Core.Domain.Accounts
// {
//   public class AccountGroup : Entity
//   {
//     [JsonConstructor]
//     public AccountGroup(Guid id, string name)
//       : base(id)
//     {
//       Name = name.Trim();
//     }
//
//     public AccountGroup(string name, IEnumerable<Account>? accounts = null)
//       : this(Guid.Empty, name)
//     {
//       Accounts = new ObservableCollectionExtended<Account>(accounts ?? Array.Empty<Account>());
//     }
//
//     public string Name { get; private set; }
//
//     public ObservableCollectionExtended<Account> Accounts { get; private set; } = new();
//
//     public void Remove(Account acc)
//     {
//       Accounts.Remove(acc);
//     }
//   }
// }