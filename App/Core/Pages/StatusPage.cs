namespace CoeurApi.App.Core.Pages;

public static class StatusPage
{
    public static string Render(IWebHostEnvironment env)
    {
        var scalarLink = env.IsDevelopment()
            ? """<a class="docs-link" href="/scalar/v1">API Reference →</a>"""
            : "";

        return $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Coeur API</title>
                <style>
                    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

                    body {
                        font-family: system-ui, -apple-system, sans-serif;
                        background: #060a12;
                        color: #e2e8f0;
                        min-height: 100vh;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }

                    .card {
                        background: rgba(15, 22, 36, 0.9);
                        border: 1px solid #1e2d45;
                        border-radius: 16px;
                        padding: 48px 52px;
                        width: min(380px, calc(100vw - 32px));
                        text-align: center;
                        box-shadow: 0 0 60px rgba(34, 211, 238, 0.05), 0 24px 48px rgba(0, 0, 0, 0.4);
                    }

                    @media (max-width: 440px) {
                        body { align-items: flex-start; padding: 40px 16px; }
                        .card { padding: 36px 28px; border-radius: 12px; }
                        .logo { font-size: 1.4rem; }
                    }

                    .logo {
                        font-size: 1.6rem;
                        font-weight: 700;
                        letter-spacing: -0.3px;
                        margin-bottom: 4px;
                    }

                    .logo .accent { color: #22d3ee; }

                    .tagline {
                        font-size: 0.7rem;
                        color: #475569;
                        letter-spacing: 0.15em;
                        text-transform: uppercase;
                        margin-bottom: 36px;
                    }

                    .divider {
                        border: none;
                        border-top: 1px solid #1e2d45;
                        margin-bottom: 28px;
                    }

                    .status-row {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        margin-bottom: 14px;
                    }

                    .label {
                        font-size: 0.75rem;
                        color: #475569;
                        text-transform: uppercase;
                        letter-spacing: 0.08em;
                    }

                    .value {
                        font-size: 0.8rem;
                        font-weight: 500;
                        color: #94a3b8;
                    }

                    .status-indicator {
                        display: flex;
                        align-items: center;
                        gap: 7px;
                    }

                    .dot {
                        width: 7px;
                        height: 7px;
                        border-radius: 50%;
                        background: #22d3ee;
                        box-shadow: 0 0 8px #22d3ee;
                        animation: glow 2s ease-in-out infinite;
                    }

                    @keyframes glow {
                        0%, 100% { box-shadow: 0 0 6px #22d3ee; }
                        50%       { box-shadow: 0 0 14px #22d3ee, 0 0 24px rgba(34, 211, 238, 0.3); }
                    }

                    .status-text {
                        font-size: 0.8rem;
                        font-weight: 600;
                        color: #22d3ee;
                    }

                    .docs-link {
                        display: inline-block;
                        margin-top: 28px;
                        font-size: 0.75rem;
                        color: #22d3ee;
                        text-decoration: none;
                        opacity: 0.7;
                        transition: opacity 0.15s;
                    }

                    .docs-link:hover { opacity: 1; }
                </style>
            </head>
            <body>
                <div class="card">
                    <div class="logo">Coeur<span class="accent">API</span></div>
                    <div class="tagline">REST Interface</div>
                    <hr class="divider">
                    <div class="status-row">
                        <span class="label">Status</span>
                        <div class="status-indicator">
                            <div class="dot"></div>
                            <span class="status-text">online</span>
                        </div>
                    </div>
                    {{scalarLink}}
                </div>
            </body>
            </html>
            """;
    }
}
