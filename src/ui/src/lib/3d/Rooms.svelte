<script lang="ts">
    import { config } from '$lib/stores';
    import * as THREE from 'three';
    import type { Group } from 'three';

    export let groupPivot: Group;

    // Position adjustments
    const X_POS_ADJ = 1.5;
    const Y_POS_ADJ = 5;

    const materials = {
        green1: new THREE.LineBasicMaterial({
            color: 0x03a062,
            transparent: true,
            opacity: 0.6
        }),
    };

    const floorMaterial = new THREE.MeshBasicMaterial({
        color: 0x03a062,
        side: THREE.DoubleSide,
        opacity: 0.1,
        transparent: true
    });

    let roomGroup: THREE.Group | null = null;

    function cleanupRooms() {
        if (roomGroup) {
            roomGroup.traverse(child => {
                if ((child as any).geometry) {
                    (child as any).geometry.dispose();
                }
                if ((child as any).material) {
                    (child as any).material.dispose();
                }
            });
            groupPivot.remove(roomGroup);
            roomGroup = null;
        }
    }

    $: if ($config?.floors && groupPivot) {
        cleanupRooms();
        const newRoomGroup = new THREE.Group();
        newRoomGroup.name = 'RoomGroup';

        $config.floors.forEach(floor => {
            const floor_base = floor.bounds[0][2];
            const floor_ceiling = floor.bounds[1][2];

            floor.rooms?.forEach((room: any) => {
            const points3d: THREE.Vector3[] = [];
            const pointsFloor: THREE.Vector2[] = [];

            room.points.forEach((points: number[]) => {
                points3d.push(new THREE.Vector3(points[0], points[1], floor_base));
                points3d.push(new THREE.Vector3(points[0], points[1], floor_ceiling));
                points3d.push(new THREE.Vector3(points[0], points[1], floor_base));

                pointsFloor.push(new THREE.Vector2(
                    points[0] - X_POS_ADJ,
                    points[1] - Y_POS_ADJ
                ));
            });

            room.points.forEach((points: number[]) => {
                points3d.push(new THREE.Vector3(points[0], points[1], floor_ceiling));
            });

                const lines = new THREE.BufferGeometry().setFromPoints(points3d);
                const roomLine = new THREE.Line(lines, materials.green1);
                roomLine.position.set(-X_POS_ADJ, -Y_POS_ADJ, 0);
                newRoomGroup.add(roomLine);

                const floorShape = new THREE.Shape(pointsFloor);
                const floorGeometry = new THREE.ShapeGeometry(floorShape);
                const plane = new THREE.Mesh(floorGeometry, floorMaterial);
                plane.position.z = floor_base;
                newRoomGroup.add(plane);
            });
        });

        groupPivot.add(newRoomGroup);
        roomGroup = newRoomGroup;
    }
</script>