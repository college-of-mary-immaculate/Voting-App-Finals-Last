import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  username: string = '';
  errorMessage: string = '';

  constructor(private router: Router) {}

  login(): void {

    const trimmedUsername = this.username.trim();

    if (!trimmedUsername) {
      this.errorMessage = 'Username is required';
      return;
    }

    
    localStorage.setItem('voter', trimmedUsername);

    
    this.router.navigate(['/vote']);
  }

}
// login.component.html

<div class="login-container">
  <h2>Voter Login</h2>

  <input 
    type="text"
    placeholder="Enter your name"
    [(ngModel)]="username"
  />

  <button (click)="login()">Login</button>

  <p *ngIf="errorMessage" class="error">
    {{ errorMessage }}
  </p>
</div>
