import { Component } from '@angular/core';
import { SearchPage } from './search/search-page';

@Component({
  selector: 'app-root',
  imports: [SearchPage],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
