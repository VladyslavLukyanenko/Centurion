export class OperationStatusMessage {
  constructor(readonly messageKey: string, readonly messageArgs?: any[]) {
  }
}

export namespace genericMessages {
  export const CREATED = new OperationStatusMessage("commonMessages.created");
  export const REMOVED = new OperationStatusMessage("commonMessages.removed");
  export const UPDATED = new OperationStatusMessage("commonMessages.updated");
  export const DONE = new OperationStatusMessage("commonMessages.done");

  export const FAILED = new OperationStatusMessage("commonMessages.failed");
}
