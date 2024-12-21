<script lang="ts">
    import { T } from '@threlte/core';
    import { HTML } from '@threlte/extras';
    import { config } from '$lib/stores';
    import type { Room, Floor } from '$lib/types';

    $: floors = $config?.floors || [];

    function getRoomCenter(points: [number, number][]): [number, number] {
        if (!points || points.length === 0) return [0, 0];

        const sum = points.reduce((acc, point) => [acc[0] + point[0], acc[1] + point[1]], [0, 0]);
        return [sum[0] / points.length, sum[1] / points.length];
    }

    function getRoomSize(points: [number, number][]): [number, number] {
        if (!points || points.length === 0) return [1, 1];

        const xs = points.map((p) => p[0]);
        const ys = points.map((p) => p[1]);
        const width = Math.max(...xs) - Math.min(...xs);
        const height = Math.max(...ys) - Math.min(...ys);
        return [Math.max(width, 1), Math.max(height, 1)];
    }

    // Height of each floor in 3D units
    const FLOOR_HEIGHT = 3;
    // Floor thickness
    const FLOOR_THICKNESS = 0.1;
    // Floor base color
    const FLOOR_COLOR = 0x808080;
</script>

<T.Group name="FloorsGroup">
    {#each floors as floor, floorIndex}
        {#each floor.rooms as room}
            {#if room.points && room.points.length > 0}
                {@const center = getRoomCenter(room.points)}
                {@const [width, height] = getRoomSize(room.points)}
                <T.Group position={[center[0], center[1], 0]}>
                    <T.Mesh>
                        <T.BoxGeometry args={[width, height, FLOOR_THICKNESS]} />
                        <T.MeshStandardMaterial color={FLOOR_COLOR} transparent={true} opacity={0.3} />
                    </T.Mesh>

                    <!-- Room label -->
                    <HTML
                        center
                        occlude
                        style="color: #ffffff;
                                   font-family: Arial;
                                   font-size: 0.8rem;
                                   margin-top: -1em;"
                    >
                        <div>{room.name}</div>
                    </HTML>
                </T.Group>
            {/if}
        {/each}
    {/each}
</T.Group>
