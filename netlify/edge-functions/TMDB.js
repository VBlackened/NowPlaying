export default async (request, context) => {
    const API_KEY = Netlify.env.API_KEY;
    const API_URL = Netlify.env.API_URL;

    if (!API_KEY || !API_URL) {
        return new Response("Missing API_KEY or API_URL", { status: 500 });
    }

    const url = new URL(request.url);

    // /TMDB/movie/popular → movie/popular
    const tmdbPath = url.pathname.replace("/TMDB/", "");

    const baseUrl = API_URL.endsWith("/")
        ? API_URL
        : API_URL + "/";

    const targetUrl = baseUrl + tmdbPath + url.search;

    const response = await fetch(targetUrl, {
        method: request.method,
        headers: {
            Authorization: `Bearer ${API_KEY}`,
        },
    });

    return new Response(response.body, {
        status: response.status,
        headers: response.headers,
    });
};