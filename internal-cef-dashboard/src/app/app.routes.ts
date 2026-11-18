// import { Routes } from '@angular/router';

// export const routes: Routes = [];
import { Routes } from '@angular/router';
import { MemberInsertComponent } from './components/member-insert.component';
//import { ClaimInsertComponent } from './components/claim-insert/claim-insert.component';

export const routes: Routes = [
  { path: 'members/new', component: MemberInsertComponent },
 // { path: 'claims/new', component: ClaimInsertComponent },
  { path: '', redirectTo: 'members/new', pathMatch: 'full' }
];
