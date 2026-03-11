import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-vote',
  templateUrl: './vote.component.html',
  styleUrls: ['./vote.component.css']
})
export class VoteComponent {

 
  candidates: string[] = ['Candidate A', 'Candidate B', 'Candidate C'];

  
  selectedCandidate: string = '';

  
  errorMessage: string = '';

  constructor(private router: Router) {}

  submitVote(): void {

    if (!this.selectedCandidate) {
      this.errorMessage = 'Please select a candidate before submitting your vote.';
      return;
    }

    
    localStorage.setItem('vote', this.selectedCandidate);

   
    this.router.navigate(['/results']);
  }

}

// vote.component.html

<div class="vote-container">
  <h2>Select Your Candidate</h2>

  <div *ngFor="let candidate of candidates">
    <label>
      <input 
        type="radio"
        name="candidate"
        [value]="candidate"
        [(ngModel)]="selectedCandidate"
      />
      {{ candidate }}
    </label>
  </div>

  <button (click)="submitVote()">Submit Vote</button>

  <p *ngIf="errorMessage" class="error">
    {{ errorMessage }}
  </p>
</div>
