const region = import.meta.env.VITE_REGION ?? 'overseas'

export function useRegion() {
  const isDomestic = region === 'domestic'

  return {
    region,
    currency: isDomestic ? 'CNY' : 'USD',
    locale: isDomestic ? 'zh-CN' : 'en-US',
    enabledPlatforms: isDomestic
      ? ['Douyin', 'Kuaishou', 'Xiaohongshu', 'BrandDeal']
      : ['YouTube', 'TikTok', 'Instagram', 'Kwai', 'Xiaohongshu', 'BrandDeal'],
  }
}
