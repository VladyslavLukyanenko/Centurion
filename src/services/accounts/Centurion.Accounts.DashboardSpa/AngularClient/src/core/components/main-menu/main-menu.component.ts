import {ChangeDetectionStrategy, Component, EventEmitter, OnInit, Output} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {IdentityService} from "../../services/identity.service";
import {MenuItem, MenuItemGroup, MenuItemsProvider} from "../../services/menu-items.provider";
import {BehaviorSubject} from "rxjs";

@Component({
  selector: "app-main-menu",
  templateUrl: "./main-menu.component.html",
  styleUrls: ["./main-menu.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainMenuComponent extends DisposableComponentBase implements OnInit {
  @Output()
  navigated = new EventEmitter<void>();

  menuItems$ = new BehaviorSubject<MenuItemGroup[]>([]);

  constructor(private id: IdentityService, private menuProvider: MenuItemsProvider) {
    super();
  }

  trackByRoute = (_: number, item: MenuItem) => item.route;

  ngOnInit(): void {
    document.addEventListener("click", e => {
      this.dispatchNavigated(e);
    }, true);

    this.id.currentUser$.subscribe(u => {
      if (!u) {
        this.menuItems$.next([]);
        return;
      }

      const items = [];
      items.push(this.menuProvider.getRegularMenuItemsSection());
      if (this.id.currentUser.isCurrentDashboardOwner) {
        items.push(this.menuProvider.getAdvancedMenuItemsSection());
      }

      this.menuItems$.next(items);
    });
  }

  private dispatchNavigated(e: MouseEvent): void {
    if ((e.target as Element)?.classList.contains("MainMenu-item")) {
      this.navigated.emit();
    }
  }
}
