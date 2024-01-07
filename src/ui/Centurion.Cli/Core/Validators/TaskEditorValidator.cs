using Centurion.Cli.Core.ViewModels.Tasks;
using FluentValidation;

namespace Centurion.Cli.Core.Validators;

public class TaskEditorValidator : AbstractValidator<TaskEditorViewModel>
{
  public TaskEditorValidator()
  {
    RuleFor(_ => _.Profile)
      .NotNull()
      .WithMessage("Profile not selected")
      .Unless(_ => _.UseRandomProfile);

    RuleFor(_ => _.ProfileGroup)
      .NotNull()
      .WithMessage("Profile Group not selected")
      .When(_ => _.UseRandomProfile);

    RuleFor(_ => _.TaskCountToCreate).GreaterThan(0).WithMessage("Invalid Qty");
    RuleFor(_ => _.SelectedModeMetadata).NotNull().WithMessage("Mode not selected");
    RuleFor(_ => _.SelectedModuleMetadata).NotNull().WithMessage("Module not selected");
  }
}