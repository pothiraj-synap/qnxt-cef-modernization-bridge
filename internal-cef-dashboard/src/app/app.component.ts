import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="dashboard-layout">
      <h1>QNXT CEF Internal Operations Dashboard</h1>
      <!-- This is where your routed component forms will render -->
      <router-outlet></router-outlet> 
    </div>
  `
})
export class AppComponent {}
