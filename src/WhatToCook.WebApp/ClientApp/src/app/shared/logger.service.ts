import { Injectable } from '@angular/core';

export abstract class LoggerService {
  abstract info: (message: string, additionalData?: unknown) => void;
  abstract warn: (message: string, additionalData?: unknown) => void;
  abstract error: (message: string, additionalData?: unknown) => void;
}

@Injectable({
  providedIn: 'root',
})
export class ConsoleLoggerService implements LoggerService {
  constructor() {}
  info(message: string, additionalData?: unknown) {
    console.info(message, additionalData);
  }

  warn(message: string, additionalData?: unknown) {
    console.warn(message, additionalData);
  }

  error(message: string, additionalData?: unknown) {
    console.error(message, additionalData);
  }
}
