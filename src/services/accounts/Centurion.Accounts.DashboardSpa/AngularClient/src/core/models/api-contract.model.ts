import {ApiError} from "../../dashboards-api";

export interface ApiContract<T> {
  error?: ApiError;
  readonly payload?: T | null;
}
