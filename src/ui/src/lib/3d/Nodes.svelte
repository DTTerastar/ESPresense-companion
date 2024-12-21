<script lang="ts">
    import { T } from '@threlte/core';
    import { HTML } from '@threlte/extras';
    import { nodes, config } from '$lib/stores';
    import type { Node } from '$lib/types';

    export let enabled = true;

    const X_POS_ADJ = 1.5;
    const Y_POS_ADJ = 5;
    const FLOOR_HEIGHT = 3;
    const NODE_SIZE = 0.15;

    const nodeMaterials = {
        online: {
            color: 0x000000,
            emissive: 0x5555ff,
            emissiveIntensity: 2,
            shininess: 100,
            toneMapped: false
        },
        offline: {
            color: 0x000000,
            emissive: 0xff2222,
            emissiveIntensity: 2,
            shininess: 100,
            toneMapped: false
        }
    };

    $: floors = $config?.floors || [];
    $: floorMap = new Map(floors.map((f, i) => [f.id, i]));

    function getNodeFloorIndex(node: Node): number {
        if (!node.floors?.length) return 0;
        return floorMap.get(node.floors[0]) || 0;
    }
</script>

{#if enabled}
    <T.Group name="NodeGroup">
        {#each $nodes as node}
            {#if node.location}
                {@const floorIndex = getNodeFloorIndex(node)}
                <T.Group
                    position={[
                        node.location.x - X_POS_ADJ,
                        node.location.y - Y_POS_ADJ,
                        floorIndex * FLOOR_HEIGHT + 0.2
                    ]}
                >
                    <!-- Node sphere -->
                    <T.Mesh name={'node#' + node.id}>
                        <T.SphereGeometry args={[NODE_SIZE, 32, 16]} />
                        <T.MeshPhongMaterial {...nodeMaterials[node.online ? 'online' : 'offline']} />
                    </T.Mesh>

                    <!-- Connection lines to show node height -->
                    <T.Mesh>
                        <T.CylinderGeometry args={[0.02, 0.02, 0.4, 8]} />
                        <T.MeshBasicMaterial
                            color={node.online ? 0x5555ff : 0xff2222}
                            transparent={true}
                            opacity={0.5}
                        />
                    </T.Mesh>

                    <!-- Node label -->
                    <HTML
                        center
                        occlude
                        position.y={0.4}
                        style="color: {node.online ? '#5555ff' : '#dc2d2d'};
                               font-family: Arial;
                               font-size: 0.8rem;
                               font-weight: bold;
                               text-shadow: 0 0 4px rgba(0,0,0,0.5);"
                    >
                        <div>{node.name}</div>
                    </HTML>
                </T.Group>
            {/if}
        {/each}
    </T.Group>
{/if}