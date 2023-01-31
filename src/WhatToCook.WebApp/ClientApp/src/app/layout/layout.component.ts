import { Component, OnInit } from '@angular/core';
import { BehaviorSubject, fromEvent, map, Observable, of, startWith, tap, throttleTime } from 'rxjs';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {
  public isNotMobile$: Observable<boolean> = of(false);
  private isSidebarVisible = new BehaviorSubject<boolean>(false);
  public isSidebarVisible$ = this.isSidebarVisible.asObservable();
  ngOnInit(): void {
    const checkScreenSize = () => document.body.offsetWidth > 767;
    this.isNotMobile$ = fromEvent(window, 'resize').pipe(map(checkScreenSize), startWith(checkScreenSize()), tap(isNotMobile => {
      if (isNotMobile) {
        this.isSidebarVisible.next(false);
      }
    }));

  }

  showMenu() {
    this.isSidebarVisible.next(!this.isSidebarVisible.value);
  }
}
