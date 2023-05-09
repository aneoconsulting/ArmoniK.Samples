const baseURL = process.env.NODE_ENV === 'production' ? '/ArmoniK.Samples/' : '/'


export default defineNuxtConfig({
  extends: "@aneoconsultingfr/armonik-docs-theme",

  app: {
    baseURL: baseURL,
    head: {
      link: [
        {
          rel: 'icon',
          type: 'image/ico',
          href: `${baseURL}favicon.ico`,
        }
      ]
    }
  },

  runtimeConfig: {
    public: {
      siteName: 'ArmoniK Samples',
      siteDescription: 'To help you to start with ArmoniK',
    }
  },
})
