import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

interface Candidate {
  id: number;
  firstName: string;
  lastName: string;
}

@Component({
  selector: 'app-vote',
  templateUrl: './vote.component.html',
  styleUrls: ['./vote.component.css']
})
export class VoteComponent implements OnInit {
  candidates: Candidate[] = [];
  selectedCandidateId: number | null = null;
  errorMessage: string = '';
  isLoading = false;

  private readonly apiBase = '/api';

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadCandidates();
  }

  async loadCandidates(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';

    try {
      const response = await fetch(`${this.apiBase}/candidate`);
      if (!response.ok) {
        throw new Error(`${response.status} ${response.statusText}`);
      }
      const data = await response.json();
      this.candidates = (data ?? []).map((c: any) => ({
        id: c.id,
        firstName: c.firstName,
        lastName: c.lastName
      }));
    } catch (error: any) {
      console.error(error);
      this.errorMessage = error?.message || 'Unable to load candidates.';
    } finally {
      this.isLoading = false;
    }
  }

  async submitVote(): Promise<void> {
    if (!this.selectedCandidateId) {
      this.errorMessage = 'Please select a candidate before submitting your vote.';
      return;
    }

    this.errorMessage = '';

    try {
      const response = await fetch(`${this.apiBase}/vote`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          electionId: 1,
          candidateId: this.selectedCandidateId
        })
      });

      if (!response.ok) {
        const text = await response.text();
        throw new Error(`${response.status} ${response.statusText}: ${text}`);
      }

      // Optionally store vote locally
      localStorage.setItem('vote', String(this.selectedCandidateId));

      this.router.navigate(['/results']);
    } catch (error: any) {
      console.error(error);
      this.errorMessage = error?.message || 'Unable to submit vote.';
    }
  }
}

// vote.component.html

<div class="vote-container">
  <h2>Select Your Candidate</h2>

  <div *ngIf="isLoading" class="loading">Loading candidates...</div>

  <div *ngFor="let candidate of candidates">
    <label>
      <input
        type="radio"
        name="candidate"
        [value]="candidate.id"
        [(ngModel)]="selectedCandidateId"
      />
      {{ candidate.firstName }} {{ candidate.lastName }}
    </label>
  </div>

  <button (click)="submitVote()">Submit Vote</button>

  <p *ngIf="errorMessage" class="error">
    {{ errorMessage }}
  </p>
</div>
