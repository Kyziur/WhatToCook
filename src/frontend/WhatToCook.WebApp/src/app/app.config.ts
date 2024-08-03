import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';

export function getBaseUrl() {
  return 'https://localhost:7059/';
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  provideRouter(routes),
  provideHttpClient(),
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),

    ...providers,
  ],
};
