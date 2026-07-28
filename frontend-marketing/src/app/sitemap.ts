import type { MetadataRoute } from "next";

const routes = ["", "/features", "/ai", "/pricing", "/security", "/solutions", "/about", "/contact", "/book-demo"];

export default function sitemap(): MetadataRoute.Sitemap {
  const base = "https://ieltslms.ai";
  return routes.map((route) => ({
    url: `${base}${route}`,
    lastModified: new Date(),
    changeFrequency: "weekly",
    priority: route === "" ? 1 : 0.7,
  }));
}
