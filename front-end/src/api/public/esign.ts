import axios from 'axios'
import type { ApiResponse } from '../../types/auth'
import type { ContractPreviewDto, SubmitSignatureRequest, SignContractResult } from '../../types/contract'

// 公开接口不携带认证 token，使用相对路径由 nginx 代理
const publicHttp = axios.create({
  baseURL: '',
  timeout: 15000,
})

export function getESignPreview(token: string) {
  return publicHttp
    .get<ApiResponse<ContractPreviewDto>>(`/v1/public/esign/${token}`)
    .then((r) => r.data.data)
}

export function submitSignature(token: string, signatureDataUrl: string) {
  return publicHttp
    .post<ApiResponse<SignContractResult>>(`/v1/public/esign/${token}`, {
      signatureDataUrl,
    } satisfies SubmitSignatureRequest)
    .then((r) => r.data.data)
}
