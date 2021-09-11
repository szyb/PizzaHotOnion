export class UserProfile {
  constructor(
    public email: string,    
    public approversMessage: string,
    public emailNotification: boolean = false
  ) { }
}
