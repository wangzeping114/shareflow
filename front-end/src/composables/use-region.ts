const region = import.meta.env.VITE_REGION ?? 'overseas'

export function useRegion() {
  const isDomestic = region === 'domestic'

  return {
    region,
    currency: isDomestic ? 'CNY' : 'USD',
    /** 客户端语言：境外英文，境内中文 */
    clientLocale: isDomestic ? 'zh-CN' : 'en-US',
    /** 管理/销售端语言：始终固定中文 */
    adminLocale: 'zh-CN' as const,
    enabledPlatforms: isDomestic
      ? ['Douyin', 'Kuaishou', 'Xiaohongshu', 'BrandDeal']
      : ['YouTube', 'TikTok', 'Instagram', 'Kwai', 'Xiaohongshu', 'BrandDeal'],
  }
}
