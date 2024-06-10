import { HttpClient } from '@angular/common/http';
import { Injectable, Provider, APP_INITIALIZER } from '@angular/core';
import { tap } from 'rxjs';

export interface AppConfig {
  defaultImagePath: string;
}

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private config?: AppConfig;

  constructor(private http: HttpClient) {}

  getConfig(): AppConfig {
    if (!this.config) {
      console.error('Application config not loaded correctly');
      throw new Error('Application config not loaded correctly');
    }

    return this.config;
  }

  loadConfig() {
    const configUrl = 'config.json';
    return this.http.get<AppConfig>(configUrl).pipe(
      tap(config => {
        this.config = config;
      })
    );
  }
}

function AppConfigInitializer(appConfigService: AppConfigService) {
  return () => appConfigService.loadConfig();
}

export const appConfigProvider: Provider[] = [
  AppConfigService,
  {
    provide: APP_INITIALIZER,
    useFactory: AppConfigInitializer,
    deps: [AppConfigService, HttpClient],
    multi: true,
  },
];
