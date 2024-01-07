import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'r-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
