import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { VehicleDto } from '../../../core/api/models';

type ViewMode = 'cards' | 'table';

@Component({
  selector: 'app-vehicle-results',
  standalone: true,
  imports: [
    DecimalPipe,
    MatButtonToggleModule,
    MatTableModule,
    MatSortModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './vehicle-results.html',
  styleUrl: './vehicle-results.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VehicleResults {
  readonly vehicles = input.required<VehicleDto[]>();

  protected readonly viewMode = signal<ViewMode>('cards');
  protected readonly columns = [
    'thumbnail',
    'title',
    'price',
    'year',
    'mileage',
    'fuelType',
    'transmission',
    'power',
    'location',
    'link'
  ];

  private readonly sortState = signal<Sort>({ active: '', direction: '' });

  protected readonly sortedVehicles = computed(() => {
    const sort = this.sortState();
    const vehicles = [...this.vehicles()];
    if (!sort.active || !sort.direction) {
      return vehicles;
    }

    const factor = sort.direction === 'asc' ? 1 : -1;
    return vehicles.sort((a, b) => {
      switch (sort.active) {
        case 'title':
          return a.title.localeCompare(b.title) * factor;
        case 'price':
          return (a.priceAmount - b.priceAmount) * factor;
        case 'year':
          return (a.productionYear - b.productionYear) * factor;
        case 'mileage':
          return ((a.mileage ?? 0) - (b.mileage ?? 0)) * factor;
        default:
          return 0;
      }
    });
  });

  protected setViewMode(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  protected onSortChange(sort: Sort): void {
    this.sortState.set(sort);
  }

  protected onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = '/vehicle-placeholder.svg';
  }
}
