import { TestBed } from '@angular/core/testing';
import { Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement } from '@angular/core';
import { TextareaAutoResizeDirective } from './textarea-auto-resize.directive';
import { By } from '@angular/platform-browser';

@Component({
  standalone: true,
  template: `
    <textarea appTextareaAutoresize></textarea>
  `,
  imports: [TextareaAutoResizeDirective],
})
class TestComponent {}

describe('TextAreaAutoResizeDirective', () => {
  let debugElements: DebugElement[] = [];
  beforeEach(() => {
    const fixture = TestBed.configureTestingModule({
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).createComponent(TestComponent);
    fixture.detectChanges(); // initial binding
    debugElements = fixture.debugElement.queryAll(
      By.directive(TextareaAutoResizeDirective)
    );
  });

  it('should create an instance', () => {
    expect(debugElements.length).toBe(1);
  });
});
