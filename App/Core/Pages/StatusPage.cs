namespace NeonVertexApi.App.Core.Pages;

public static class StatusPage
{
    public static string Render(IWebHostEnvironment env)
    {
        var scalarLink = env.IsDevelopment()
            ? """<p><a href="/scalar/v1">→ API Reference</a></p>"""
            : "";

        return $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>NeonVertex API</title>
                <style>
                    body { font-family: system-ui, sans-serif; max-width: 480px; margin: 80px auto; padding: 0 20px; color: #111; }
                    h1 { font-size: 1.4rem; margin-bottom: .5rem; }
                    .badge { display: inline-block; padding: 2px 10px; border-radius: 4px; font-size: .75rem; font-weight: 600; }
                    .ok { background: #d1fae5; color: #065f46; }
                    .ev { background: #e0e7ff; color: #3730a3; }
                    p { margin-top: 1.5rem; font-size: .875rem; color: #6b7280; }
                    a { color: #4f46e5; }
                </style>
            </head>
            <body>
                <h1>NeonVertex API</h1>
                <span class="badge ok">● online</span>
                &nbsp;
                <span class="badge ev">{{env.EnvironmentName}}</span>
                {{scalarLink}}
            </body>
            </html>
            """;
    }
}
