export class OperationStatusMessage {
  static readonly CREATED = new OperationStatusMessage("Created successfully");
  static readonly REMOVED = new OperationStatusMessage("Removed successfully");
  static readonly UPDATED = new OperationStatusMessage("Updated successfully");
  static readonly DONE = new OperationStatusMessage("Operation finished successfully");

  static readonly FAILED = new OperationStatusMessage("Operation failed");

  constructor(readonly messageKey: string, readonly messageArgs?: any[]) {
  }
}
