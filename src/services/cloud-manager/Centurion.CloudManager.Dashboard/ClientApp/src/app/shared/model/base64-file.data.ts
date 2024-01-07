export interface IBinaryData {
  readonly name?: string | null;
  readonly contentType?: string | null;
  readonly length?: number;
}

export interface Base64FileData extends IBinaryData {
  content?: string;
}
