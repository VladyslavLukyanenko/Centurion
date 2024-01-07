// using System;
// using Centurion.Cli.Core.Domain.Accounts;
//
// namespace Centurion.Cli.Core.Domain.Fields
// {
//   public class AccountPickerField : Field<AccountGroup>
//   {
//     private int _lastIdx;
//
//     public AccountPickerField()
//     {
//     }
//
//     public AccountPickerField(string displayName)
//       : this(null, displayName, true)
//     {
//     }
//     
//     public AccountPickerField(string? systemName, string displayName, bool isRequired)
//       : base(systemName, displayName, isRequired)
//     {
//     }
//
//     public Account GetNextAccount()
//     {
//       if (Value is null || Value.Accounts.Count == 0)
//       {
//         throw new InvalidOperationException("Account group has no accounts");
//       }
//
//       if (_lastIdx >= Value.Accounts.Count)
//       {
//         _lastIdx = 0;
//       }
//
//       return Value.Accounts[_lastIdx++];
//     }
//
//     public override string? ValueId => Value?.Id.ToString();
//     public override string? DisplayValue => Value?.Name;
//   }
// }