// Minimal ambient typing for the 3 methods this app actually uses from Google Identity Services
// (loaded via the <script> tag in index.html) — not worth a full @types dependency for this.
export {};

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize(config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
          }): void;
          renderButton(parent: HTMLElement, options?: Record<string, unknown>): void;
          prompt(): void;
        };
      };
    };
  }
}
