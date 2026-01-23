export default async (request, context) => {
    console.log("Edge function called:", request.url);

    const API_KEY = context.env.get('API_KEY');
    const API_URL = context.env.get('API_URL');

    console.log("API_URL:", API_URL);
    console.log("Has API_KEY:", !!API_KEY);

    if (!API_KEY || !API_URL) {
        console.error("Missing credentials");
        return new Response("Missing API_KEY or API_URL", { status: 500 });
    }

    const url = new URL(request.url);
    const tmdbPath = url.pathname.replace("/TMDB/", "");
    const baseUrl = API_URL.endsWith("/") ? API_URL : API_URL + "/";
    const targetUrl = baseUrl + tmdbPath + url.search;

    console.log("Proxying to:", targetUrl);

    try {
        const response = await fetch(targetUrl, {
            method: request.method,
            headers: {
                Authorization: `Bearer ${API_KEY}`,
            },
        });

        console.log("Response status:", response.status);

        return new Response(response.body, {
            status: response.status,
            headers: response.headers,
        });
    } catch (error) {
        console.error("Fetch error:", error);
        return new Response("Proxy error", { status: 500 });
    }
};