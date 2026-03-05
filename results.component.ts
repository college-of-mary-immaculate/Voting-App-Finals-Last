import { Component } from '@angular/core';

@Component({
  selector: 'app-results',
  templateUrl: './results.component.html'
})
export class ResultsComponent {

  votedFor = localStorage.getItem('vote');

}

//Run mo nalang pre
// ng serve
// tapos open mo http://localhost:4200
