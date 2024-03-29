/**
 * Monitors Panel API
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { SupportedHostingTargets } from './supported-hosting-targets.model';
import { ImageRuntimeInfo } from './image-runtime-info.model';
import { ServerInstanceStatus } from './server-instance-status.model';


export interface ServerInstance { 
    readonly id?: string | null;
    publicDnsName?: string | null;
    readonly dockerRemoteApiUrl?: string | null;
    supportedHostingTargets?: SupportedHostingTargets;
    readonly isAvailable?: boolean;
    readonly lastChecked?: string;
    readonly providerName?: string | null;
    status?: ServerInstanceStatus;
    additionalStats?: { [key: string]: string; } | null;
    readonly images?: Array<ImageRuntimeInfo> | null;
    readonly isRunning?: boolean;
    readonly isStopped?: boolean;
    readonly isIdle?: boolean;
}

