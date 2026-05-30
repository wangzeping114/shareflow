const region = import.meta.env.VITE_REGION ?? 'overseas'

export function useRegion() {
  const isDomestic = region === 'domestic'
  const defaultClientLocale = isDomestic ? 'zh-CN' : 'en-US'

  return {
    region,
    currency: isDomestic ? 'CNY' : 'USD',
    /** 客户端默认语言：境外英文，境内中文 */
    clientLocale: defaultClientLocale,
    /** 客户端支持语言（中/英/日/韩/越南） */
    clientSupportedLocales: ['zh-CN', 'en-US', 'ja-JP', 'ko-KR', 'vi-VN'] as const,
    /** 管理/销售端语言：始终固定中文 */
    adminLocale: 'zh-CN' as const,
    enabledPlatforms: isDomestic
      ? ['Douyin', 'Kuaishou', 'Xiaohongshu', 'BrandDeal']
      : ['YouTube', 'TikTok', 'Instagram', 'Kwai', 'Xiaohongshu', 'BrandDeal'],
  }
}
