import { Injectable } from '@angular/core'; 
import { CanActivate, Router,ActivatedRouteSnapshot } from '@angular/router'; 
import { OAuthService } from 'angular-oauth2-oidc';
import {TokenStorageService} from 'src/app/core/services/token-storage.service'
 @Injectable({ providedIn: 'root' }) 
 export class AuthGuard implements CanActivate 
 { constructor
  ( private oauthService: OAuthService, 
    private router: Router,
    private tokenStorage: TokenStorageService 
  ) {} 
 canActivate(route :ActivatedRouteSnapshot): boolean 
 { 
  if (!this.oauthService.hasValidAccessToken()) 
  { 
    this.oauthService.initLoginFlow();
    return false; 
  } 
  const requiredRoles=route.data?.['roles']as string[];
  if(!requiredRoles || requiredRoles.length ===0){
    return true;
  }
  const userRole=this.tokenStorage.getRole();
  if(requiredRoles.includes(userRole)){
    return true;
  }
  this.router.navigate(['/dashboard']);
  return false;
}
 }