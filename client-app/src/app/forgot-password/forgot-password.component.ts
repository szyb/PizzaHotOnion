import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../shared/auth/authentication.service';

@Component({
  selector: 'app-root',
  providers: [AuthenticationService],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  public email: string;
  public isSubmitted: boolean = false;

  constructor(
    public router: Router,
    private authenticationService: AuthenticationService) {
  }

  submit() {
    this.resetPassword();
  }

  resetPassword() {
    this.authenticationService.resetPassword(this.email).subscribe(
      result => {
        this.isSubmitted = true;
      }
    );

  }
}
