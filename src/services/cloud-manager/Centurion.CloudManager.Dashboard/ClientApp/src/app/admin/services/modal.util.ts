import {NzModalRef} from "ng-zorro-antd";

export const executeBlocking = async (modal: NzModalRef<any>, p: () => Promise<any>, onError: (details?: string) => void) => {
  modal.updateConfig({
    nzOkLoading: true,
    nzCancelDisabled: true,
    nzClosable: false
  });
  try {
    await p();
  } catch (e) {
    let details = null;
    if (e.error && e.error.message) {
      details = e.error.message;
    }

    onError(details);
  } finally {
    modal.updateConfig({
      nzOkLoading: false,
      nzCancelDisabled: false,
      nzClosable: true
    });
  }
};
